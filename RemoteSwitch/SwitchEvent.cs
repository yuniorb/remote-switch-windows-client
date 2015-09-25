namespace RemoteSwitchClient
{
    using System;

    public class SwitchEvent
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime EventDate { get; set; }

        public bool Status { get; set; }

        public override bool Equals(object obj)
        {
            var value = (SwitchEvent) obj;
            return value != null && value.Id.Equals(this.Id, StringComparison.OrdinalIgnoreCase);
        }

        protected bool Equals(SwitchEvent other)
        {
            return string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return (Id != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Id) : 0);
        }
    }
}