namespace PvPmo.Resources.Selectors
{
    public class ConfigDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? NormalizzaTemplate { get; set; }
        public DataTemplate? OrigineTemplate { get; set; }
        public DataTemplate? ProgressTemplate { get; set; }
        public DataTemplate? FinalizzaTemplate { get; set; }
        public DataTemplate? UtentiTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                NormalizzaConfig when NormalizzaTemplate is not null => NormalizzaTemplate,
                OrigineConfig when OrigineTemplate is not null => OrigineTemplate,
                ProgressConfig when ProgressTemplate is not null => ProgressTemplate,
                FinalizzaConfig when FinalizzaTemplate is not null => FinalizzaTemplate,
                Utente when UtentiTemplate is not null => UtentiTemplate,
                _ => new DataTemplate(() =>
                    new Label
                    {
                        Text = "Tipo di configurazione non gestito.",
                        TextColor = Colors.Red,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(10)
                    })
            };
        }
    }
}
