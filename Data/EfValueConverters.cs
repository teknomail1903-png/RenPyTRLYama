using System;
using System.Collections.Generic;
using System.Linq;

namespace RenPyTRLauncher.Data
{
    public static class EfValueConverters
    {
        public static string StringListToDb(List<string>? value)
            => string.Join(";", value ?? new List<string>());

        public static List<string> StringListFromDb(string? value)
            => (value ?? string.Empty)
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

        public static string GuidListToDb(List<Guid>? value)
            => string.Join(";", (value ?? new List<Guid>()).Select(g => g.ToString()));

        public static List<Guid> GuidListFromDb(string? value)
        {
            var result = new List<Guid>();
            foreach (var part in (value ?? string.Empty).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (Guid.TryParse(part, out var id))
                    result.Add(id);
            }
            return result;
        }
    }
}
