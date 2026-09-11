using ContractPulseAI.API.Models.Dtos;
using ContractPulseAI.API.Models.Entities;
using ContractPulseAI.API.Repositories.Interface;
using ContractPulseAI.API.Services.Interface;

namespace ContractPulseAI.API.Services.Implementation
{
    public class RfpService : IRfpService
    {
        private readonly IRfpRepository _repository;

        public RfpService(IRfpRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ClientRfpDto>> GetAllRfpsAsync()
        {
            var rfps = await _repository.GetAllAsync();

            return rfps.Select(rfp => new ClientRfpDto
            {
                Id = rfp.ID,
                ClientName = rfp.Client_Name,
                ClientEmail = rfp.Client_Email,
                RfpPrompt = rfp.RFP_Prompt,
                RfpLink = rfp.RFP_Link,
                SowLink = rfp.SOW_Link,
                RfpStatus = rfp.RFP_Status,
                CreatedDate = rfp.Created_Date,
                LastUpdatedDate = rfp.LastUpdatedDate
            }).ToList();
        }

        public async Task<ClientRfpDto?> GetRfpByIdAsync(int id)
        {
            var rfp = await _repository.GetByIdAsync(id);

            if (rfp == null)
                return null;

            return new ClientRfpDto
            {
                Id = rfp.ID,
                ClientName = rfp.Client_Name,
                ClientEmail = rfp.Client_Email,
                RfpPrompt = rfp.RFP_Prompt,
                RfpLink = rfp.RFP_Link,
                SowLink = rfp.SOW_Link,
                RfpStatus = rfp.RFP_Status,
                CreatedDate = rfp.Created_Date,
                LastUpdatedDate = rfp.LastUpdatedDate
            };
        }

        public async Task<ClientRfpDto> CreateRfpAsync(ClientRfpDto dto)
        {
            var entity = new ClientRFP
            {
                Client_Name = dto.ClientName,
                Client_Email = dto.ClientEmail,
                RFP_Prompt = dto.RfpPrompt,
                RFP_Link = dto.RfpLink,
                SOW_Link = dto.SowLink,
                RFP_Status = dto.RfpStatus,
                Created_Date = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            };

            entity = await _repository.CreateAsync(entity);

            return new ClientRfpDto
            {
                Id = entity.ID,
                ClientName = entity.Client_Name,
                ClientEmail = entity.Client_Email,
                RfpPrompt = entity.RFP_Prompt,
                RfpLink = entity.RFP_Link,
                SowLink = entity.SOW_Link,
                RfpStatus = entity.RFP_Status,
                CreatedDate = entity.Created_Date,
                LastUpdatedDate = entity.LastUpdatedDate
            };
        }
    }
}
