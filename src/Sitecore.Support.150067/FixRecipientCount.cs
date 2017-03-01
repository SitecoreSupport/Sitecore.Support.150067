
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.EmailCampaign.Server;
using Sitecore.EmailCampaign.Server.Contexts;
using Sitecore.EmailCampaign.Server.Responses;
using Sitecore.EmailCampaign.Server.Services;
using Sitecore.EmailCampaign.Server.Services.Interfaces;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Services.Core;
using Sitecore.Services.Infrastructure.Web.Http;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Sitecore.ListManagement.ContentSearch.Model;
using System.Linq;
using Sitecore.EmailCampaign.Server.Controllers;


namespace Sitecore.Support.EmailCampaign.Server.Controllers.DataSource
{
    public class CustomRecipientListsController : ServicesApiController
    {
        private readonly IRecipientListService recipientListService;

        private readonly IRecipientService recipientService;

        private readonly ILogger logger;


        public CustomRecipientListsController() : this(new RecipientListService(), new RecipientService(), Logger.Instance)
        {
        }

        public CustomRecipientListsController(IRecipientListService recipientListService, IRecipientService recipientService, ILogger logger)
        {
            Assert.ArgumentNotNull(recipientListService, "recipientListService");
            Assert.ArgumentNotNull(recipientListService, "recipientService");
            Assert.ArgumentNotNull(logger, "logger");
            this.recipientListService = recipientListService;
            this.recipientService = recipientService;
            this.logger = logger;
        }

        [ActionName("DefaultAction")]
        public Response RecipientLists(RecipientListDataSourceContext data)
        {
            Assert.ArgumentNotNull(data, "data");
            Assert.IsNotNull(data.MessageId, "Could not get the message Id for data: {0}", new object[]
            {
                data
            });
            RecipientListResponse recipientListResponse = new RecipientListResponse();
            try
            {

                ID messageId = new ID(data.MessageId);
                List<Sitecore.EmailCampaign.Server.Model.RecipientList> list = this.recipientListService.Get(messageId, data.Type);
                recipientListResponse.RecipientLists = list.ToArray();
                ListManagement.ListManager<ContactList, ContactData> listManager = Sitecore.Configuration.Factory.CreateObject("contactListManager", false) as ListManagement.ListManager<ContactList, ContactData>;
                foreach (Sitecore.EmailCampaign.Server.Model.RecipientList lis in list)
                {

                    if (lis.Type == "Segmented list")
                    {
                        ContactList segList = listManager.FindById(lis.Id);
                        lis.Recipients = listManager.GetContacts(segList).Count<ContactData>();
                    }

                }

                recipientListResponse.TotalCount = this.recipientListService.GetRecipientListsCount(messageId, data.Type);
                recipientListResponse.IsUncommittedRead = this.recipientListService.HasUncommittedRecipientLists(messageId);
                recipientListResponse.TotalRecipientsCount = this.recipientService.GetRecipientsCount(messageId, data.Type);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message, ex);
                recipientListResponse.Error = true;
            }
            return recipientListResponse;
        }
    }
}
