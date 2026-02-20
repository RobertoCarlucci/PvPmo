namespace PvPmo.Util
{
    public class ConfigTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? StringTemplate { get; set; }
        public DataTemplate? DateTemplate { get; set; }
        public DataTemplate? DefaultTemplate { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            if (item is FileSetupConfig config)
            {
                // Per ora usiamo sempre il DefaultTemplate
                // Puoi estendere questa logica per selezionare template diversi
                // basandoti su proprietà specifiche di FileSetupConfig
                return DefaultTemplate;
            }

            return DefaultTemplate;
        }
    }
}
