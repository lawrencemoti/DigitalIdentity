using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationCore.Models.Infrastructure.Datanamix;

namespace ApplicationCore.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<ProfileResponse.ConsumerDetail, ApplicationCore.Models.Identity>()
                .ForMember(dest => dest.IdentityNumber, opt => opt.MapFrom(src => src.IDNumber))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.SecondName, opt => opt.MapFrom(src => src.SecondName))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                .ForMember(dest => dest.DeceasedStatus, opt => opt.MapFrom(src => src.DeceasedStatus))
                .ForMember(dest => dest.IDIssuedDate, opt => opt.MapFrom(src => src.IDIssuedDate))
                .ForMember(dest => dest.VendorEnquiryID, opt => opt.MapFrom(src => src.EnquiryID))
                .ForMember(dest => dest.VendorEnquiryResultID, opt => opt.MapFrom(src => src.EnquiryResultID));

            CreateMap<HomeAffairsIDVCheck.RealTimeIDVerification, ApplicationCore.Models.Identity>()
                .ForMember(dest => dest.IdentityNumber, opt => opt.MapFrom(src => src.IDNO))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.DeceasedStatus, opt => opt.MapFrom(src => src.DeceasedStatus))
                .ForMember(dest => dest.MaritalStatus, opt => opt.MapFrom(src => src.MaritalStatus))
                .ForMember(dest => dest.IdentityNumberMatchStatus, opt => opt.MapFrom(src => src.IDNOMatchStatus))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.HomeAffairsIDBlocked, opt => opt.MapFrom(src => src.IDBlocked))
                .ForMember(dest => dest.CountryOfBirth, opt => opt.MapFrom(src => src.CountryOfBirth))
                .ForMember(dest => dest.Citizenship, opt => opt.MapFrom(src => src.Citizenship));

        }
    }
}
