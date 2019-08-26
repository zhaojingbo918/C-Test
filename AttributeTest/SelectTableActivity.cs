using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows;
using ABCFT.ParsingPro.Activity.Core;
using ABCFT.ParsingPro.Activity.ParsingAI.Properties;
using ABCFT.ParsingPro.Activity.ParsingAI.SelectTables;
using Newtonsoft.Json;

namespace ABCFT.ParsingPro.Activity.ParsingAI
{
    [DesignerIcon(typeof(SelectTableActivity), "icons.selectTable.png")]
    [ToolboxBitmap(typeof(SelectTableActivity), "icons.selectTable.png")]
    [ActivityDefinitionExport("SelectTableActivityName", "ABCFT.ParsingPro.Activity.ParsingAI.Properties.Resource",
        typeof(SelectTableActivity), @"ParsingAI\Table")]
    public class SelectTableActivity : CodeActivity, IActivityDefinition, IInteractiveActivity
    {
        private AutoResetEvent _autoResetEvent;
        private SelectTablesUI _view;
        #region IInteractiveActivity 属性

        [Browsable(false)]
        public Action Executing
        {
            get; set;
        }

        [Browsable(false)]
        public Action Executed
        {
            get; set;
        }

        public bool CreateViewNeeded
        {
            get { return true; }
        }
        #endregion

        [Category("Input")]
        [Description("FileId")]
        public InArgument<string> FileId { get; set; }

        [Category("Input")]
        [Description("RegionInclude")]
        public InArgument<string> RegionInclude { get; set; }

        [Category("Input")]
        [Description("RegionExclude")]
        public InArgument<string> RegionExclude { get; set; }

        [Category("Input")]
        [Description("RegionAboveInclude")]
        public InArgument<string> RegionAboveInclude { get; set; }

        [Category("Input")]
        [Description("RegionAboveExclude")]
        public InArgument<string> RegionAboveExclude { get; set; }

        [Category("Output")]
        [Description("TableIds")]
        public OutArgument<string[]> TableIds { get; set; }

        static SelectTableActivity()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(SelectTableActivity), "RegionInclude", new EditorAttribute(typeof(StringListEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(SelectTableActivity), "RegionExclude", new EditorAttribute(typeof(StringListEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(SelectTableActivity), "RegionAboveInclude", new EditorAttribute(typeof(StringListEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(SelectTableActivity), "RegionAboveExclude", new EditorAttribute(typeof(StringListEditor), typeof(PropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
        public SelectTableActivity()
        {
           this.DisplayName = Resource.ResourceManager.GetString("SelectTableActivityName", CultureInfo.CurrentCulture);
        }

        private List<List<string>> GetListString(CodeActivityContext context, InArgument<string> s)
        {
            var str = s.Get(context);
            if (!string.IsNullOrEmpty(str))
            {
                return JsonConvert.DeserializeObject<List<List<string>>>(str);
            }

            return null;
        }

        // 如果活动返回值，则从 CodeActivity<TResult>
        // 并从 Execute 方法返回该值。
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                _autoResetEvent = new AutoResetEvent(false);
                Executing?.Invoke();
                // 获取 Text 输入参数的运行时值
                string materialId = context.GetValue(this.FileId);

                TablePositioningOptions options = new TablePositioningOptions()
                {
                    RegionInclude = this.GetListString(context, this.RegionInclude),
                    RegionExclude = this.GetListString(context, this.RegionExclude),
                    RegionAboveInclude = this.GetListString(context, this.RegionAboveInclude),
                    RegionAboveExclude = this.GetListString(context, this.RegionAboveExclude),
                };

                if (_view != null)
                {
                    _view.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _view.SetData(options, materialId, _autoResetEvent);
                    }));
                }
                else
                {
                    //todo:等设计器实现了显示执行时UI的功能后去掉else部分的代码
                    Thread thread = new Thread((ThreadStart)delegate
                    {
                        Window window = new Window() { Title = "表格选择" };
                        _view = new SelectTablesUI();
                        _view.SetData(options, materialId, _autoResetEvent, window);
                        window.Content = _view;
                        window.ShowDialog();
                        if (_autoResetEvent != null)
                        {
                            _autoResetEvent.Set();
                        }
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                }
                _autoResetEvent.WaitOne();
                _autoResetEvent.Dispose();
                _autoResetEvent = null;
                this.TableIds.Set(context, _view.GetTables());
            }
            catch (Exception ex)
            {
                //todo: log exception
                throw;
            }
            finally
            {
                _view = null;
                Executed?.Invoke();
            }
        }



        public IEnumerable<ViewBindingInfo> CreateViewBindings()
        {
            _view = new SelectTablesUI();
            var bindingInfo = new ViewBindingInfo() { View = _view };
            return new List<ViewBindingInfo>() { bindingInfo };
        }
    }

    internal class TablePositioningOptions
    {
        [JsonProperty("regionInclude")]
        public List<List<string>> RegionInclude { get; set; }

        [JsonProperty("regionExclude")]
        public List<List<string>> RegionExclude { get; set; }

        [JsonProperty("regionAboveInclude")]
        public List<List<string>> RegionAboveInclude { get; set; }

        [JsonProperty("regionAboveExclude")]
        public List<List<string>> RegionAboveExclude { get; set; }

    }
}
