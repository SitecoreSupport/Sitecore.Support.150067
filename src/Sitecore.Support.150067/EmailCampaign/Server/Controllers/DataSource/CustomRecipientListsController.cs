using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.EmailCampaign.Server.Contexts;
using Sitecore.EmailCampaign.Server.Model;
using Sitecore.EmailCampaign.Server.Responses;
using Sitecore.EmailCampaign.Server.Services;
using Sitecore.EmailCampaign.Server.Services.Interfaces;
using Sitecore.ListManagement;
using Sitecore.ListManagement.ContentSearch.Model;
using Sitecore.Modules.EmailCampaign.Diagnostics;
using Sitecore.Services.Core;
using Sitecore.Services.Infrastructure.Web.Http;

namespace Sitecore.Support.EmailCampaign.Server.Controllers.DataSource
{
    [Sitecore.Support.EmailCampaign.Server.Filters.SitecoreAuthorize(new string[] { @"sitecore\ECM Advanced Users", @"sitecore\ECM Users" }), ServicesController("EXM.CustomRecipientLists")]
    public class CustomRecipientListsController : ServicesApiController
    {
        private readonly ILogger logger;
        private readonly IRecipientListService recipientListService;
        private readonly IRecipientService recipientService;

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
            Assert.IsNotNull(data.MessageId, "Could not get the message Id for data: {0}", new object[] { data });
            RecipientListResponse response = new RecipientListResponse();
            try
            {
                ID messageId = new ID(data.MessageId);
                response.RecipientLists = this.recipientListService.Get(messageId, data.Type, data.PageSize, data.PageIndex).ToArray();
                ListManager<ContactList, ContactData> manager = Factory.CreateObject("contactListManager", false) as ListManager<ContactList, ContactData>;
                Assert.IsNotNull(manager,"manager is null");
                long totalCount = 0;
                foreach (RecipientList list2 in response.RecipientLists)
                {
                    if (list2.Type == "Segmented list")
                    {
                        ContactList list3 = manager.FindById(list2.Id);
                        list2.Recipients = manager.GetContacts(list3).Count<ContactData>();
                        totalCount += list2.Recipients;
                    }
                }

                response.TotalCount = this.recipientListService.GetRecipientListsCount(messageId, data.Type);
                response.IsUncommittedRead = this.recipientListService.HasUncommittedRecipientLists(messageId);
                response.TotalRecipientsCount = totalCount;
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception.Message, exception);
                response.Error = true;
            }
            return response;
        }
    }
}
