# Digital Identity API on AWS Fargate (Region:af-south-1)

A production‑ready reference that deploys a .NET 8 Digital Identity API to **AWS Fargate** behind **API Gateway (HTTP API) → VPC Link → internal ALB**, storing state in **Aurora MySQL**. Asynchronous verification flows use **SNS → SQS (buffer) → ECS consumer** to call 3rd‑party providers and update the database, then **callback** the requestor.

> Region: **af-south-1 (Cape Town)**. Two AZs for high availability.

---

## 1) Architecture Overview

```
Clients
  │ HTTPS
  ▼
API Gateway (HTTP API)
  │  (VPC Link - private integration)
  ▼
Internal ALB (private subnets)
  │
  ▼
ECS Fargate Service (API containers) ─► Aurora MySQL (private subnets)[Multi-AZ]
  │ API writes         ▲
  │ PENDING +          │ EF Core
  │ Publishes SNS      │
  ├──────────────► SNS Topic ──► Rule ──► SQS Queue (DLQ attached)
  │                                             │
  │                               Poll via consumer BackgroundService (on Fargate)
  │                                             │  calls 3rd‑party API (via NAT)
  │                                             │           │                                             │  
  │                                             ▼
  └────────────────────────────────────────── Update Aurora DB & POST callback to requestor
```

Why this setup
- API Gateway is the internet front door (for public egress); the **ALB is internal** to keep workloads private. HTTP API supports private integration to **ALB** via VPC Link.
- **SNS** fans out to, **SQS (with DLQ) target** for durable buffering and high throughput; A Fargate **DotNet BackgroundService Worker** consumer polls SQS to process messages reliably.
- Two private subnets (one per AZ) host Fargate tasks and Aurora. Two public subnets host IGW/NAT/elastic IPs.

---

## 2) API Capabilities

- **POST /identity**: accept a single identity request → create DB record with `Pending` status → publish `IdentityVerificationRequested` message on SNS → return 202 with tracking id.
- **POST /identity/batch**: accept array of identity requests → create records → publish SNS messages per item → return 202 + batch id.
- **Webhook Callback**: once validation completes, service `POST`s back to provided `callbackUrl` with unified result and provider breakdown.
- **Asynchronous Worker** (BackgroundService): consumes SQS messages emitted from SNS rule; for each identity id, calls **2+ 3rd‑party** providers in parallel with timeouts/retries, persists decision `Verified or Failed` and provider payloads, then performs callback to return status.

---