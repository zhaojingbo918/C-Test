using System;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttributeTest
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            AttributeTableBuilder attributeTableBuilder = new AttributeTableBuilder();
            SetCategories(attributeTableBuilder);
            AddDisplayname(attributeTableBuilder);

            MetadataStore.AddAttributeTable(attributeTableBuilder.CreateTable());
        }

        private void SetCategories(AttributeTableBuilder builder)
        {
            //builder.AddCustomAttributes(typeof(ExcelInsertRowActivity), new CategoryAttribute("Excel"));
        }

        private void AddDisplayname(AttributeTableBuilder builder)
        {
            builder.AddCustomAttributes(typeof(ExcelInsertRowActivity), new DisplayNameAttribute("插入行"));
        }
    }
}
