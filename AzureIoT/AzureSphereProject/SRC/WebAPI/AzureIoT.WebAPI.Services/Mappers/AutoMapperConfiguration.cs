using AutoMapper;
using AzureIoT.WebAPI.Data;
using AzureIoT.WebAPI.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureIoT.WebAPI.Services.Mappers
{
    public static class AutoMapperConfiguration
    {
        private static bool _alreadyConfigured;

        public static void Configure()
        {
            if (_alreadyConfigured)
            {
                return;
            }

            else
            {
                _alreadyConfigured = true;
            }

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<HumidityEntity, HumidityData>()
                    .ForPath(dest => dest.Id, opt => opt.MapFrom(src => src.PartitionKey))
                    .ForPath(dest => dest.Humidity, opt => opt.MapFrom(src => src.RowKey));
                cfg.CreateMap<TemperatureEntity, TemperatureData>()
                    .ForPath(dest => dest.Id, opt => opt.MapFrom(src => src.PartitionKey))
                    .ForPath(dest => dest.Temperature, opt => opt.MapFrom(src => src.RowKey));
            });

        }
    }
}
