using Swashbuckle.AspNetCore.Filters;

namespace mcqTestBackend.Utility
{
    public class DynamicJsonExample : IExamplesProvider<object>
    {
        public object GetExamples()
        {
            return new
            {
                name = "A-Boy",
                age = 22,
                email = "a-boy@example.com",
                address = new
                {
                    street = "123 Main St",
                    city = "Digha",
                    zip = "70001"
                }
            };
        }
    }
}
