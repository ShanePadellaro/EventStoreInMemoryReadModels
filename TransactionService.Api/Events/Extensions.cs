namespace TransactionService.Api.Events
{
    public static class Extensions
    {
        public static string ToCamelCase(this DomainEventTypes eventType)
        {
            var array = eventType.ToString().ToCharArray();
            array[0] = char.ToLowerInvariant(array[0]);
            return new string(array);
        }
    }
}