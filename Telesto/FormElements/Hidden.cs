namespace Telesto.FormElements
{

    internal class Hidden : FormElement
    {

        internal override bool Reported => true;
        public override string Value { get; set; }

        public override void Render()
        {
        }

    }

}
