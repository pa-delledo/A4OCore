namespace A4OCore.BLCore
{
    public class OptionSetA4O
    {
        public string Code { get; set; }
        public string Label { get; set; }
    }

    public class OptionsSetA4O : List<OptionSetA4O>
    {
        //public virtual Func<OptionSetA4O, bool> Visible = (o) => true;
        public virtual Func<OptionSetA4O, bool> Visible => (o) => true;
        public List<OptionSetA4O> GetVisible()
        {
            return this.Where(x => Visible(x)).ToList();
        }
    }
}
