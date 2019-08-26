using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttributeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            new DesignerMetadata().Register();


            var attributes = TypeDescriptor.GetAttributes(typeof(ExcelInsertRowActivity)).OfType<Attribute>().ToList();
            var displayNameAttribute = attributes.FirstOrDefault(p => p is DisplayNameAttribute);
            if (displayNameAttribute != null)
            {
                Console.WriteLine((displayNameAttribute as DisplayNameAttribute).DisplayName);
            }

            displayNameAttribute = attributes.FirstOrDefault(p => p is CategoryAttribute);
            if (displayNameAttribute != null)
            {
                Console.WriteLine((displayNameAttribute as CategoryAttribute).Category);
            }

            Console.ReadKey();
        }
    }
}
