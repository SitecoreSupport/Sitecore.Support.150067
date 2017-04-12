/* Sitecore.Support.130346\src\Sitecore.Support.130346\Hooks\UpdateNumberFieldDefinitionItem.cs */

using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Events.Hooks;
using Sitecore.SecurityModel;

namespace Sitecore.Support.Hooks
{
    public class UpdateExmItems : IHook
    {
        public void Initialize()
        {
            using (new SecurityDisabler())
            {
                var databaseName = "core";
                var itemPath = "/sitecore/client/Applications/ECM/Pages/Messages/OneTime/PageSettings/Tabs/Recipients/SubTabs/IncludeRecipients";
                var itemPath1 = "/sitecore/client/Applications/ECM/Pages/Messages/OneTime/PageSettings/Tabs/Recipients/SubTabs/ExcludeRecipients";
                var fieldName = "__renderings";

                var database = Factory.GetDatabase(databaseName);
                var item = database.GetItem(itemPath);
                var field = item[fieldName];

                if (field.Contains("Request=EXM%2fRecipientLists"))
                {
                    Log.Info("Installing the Sitecore.Support.150067", this);
                    item.Editing.BeginEdit();
                    item[fieldName] = field.Replace("Request=EXM%2fRecipientLists", "Request=EXM%2fCustomRecipientLists");
                    item.Editing.EndEdit();
                }


                item = database.GetItem(itemPath1);
                field = item[fieldName];

                if (field.Contains("Request=EXM%2fRecipientLists"))
                {
                    Log.Info("Installing the Sitecore.Support.150067", this);
                    item.Editing.BeginEdit();
                    item[fieldName] = field.Replace("Request=EXM%2fRecipientLists", "Request=EXM%2fCustomRecipientLists");
                    item.Editing.EndEdit();
                }
            }
        }
    }
}