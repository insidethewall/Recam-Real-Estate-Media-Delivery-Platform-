using System.Globalization;
using DnsClient.Protocol;
using RecamSystemApi.Models;


public static class ListingCaseDiff
{
    private const double DoubleEps = 1e-9;
    private static readonly decimal DecimalEps = 0.0000001m;

    public static List<FieldChange> Diff(ListingCase before, ListingCase after)
    {
        var changes = new List<FieldChange>();
        void Add(string field, object? oldV, object? newV) => changes.Add(new FieldChange(field, ToInv(oldV), ToInv(newV)));
        bool DiffStr(string field, string? dtoVal, string? oldVal)
        {
            if (dtoVal is null) return false; // null means "not provided" => skip
            // normalize (trim) if you like
            var nv = dtoVal.Trim();
            var ov = oldVal?.Trim();
            if (!string.Equals(nv, ov, StringComparison.Ordinal))
            {
                Add(field, ov, nv);
                return true;
            }
            return false;
        }

        bool DiffInt(string field, int dtoVal, int oldVal)
        {
            if (dtoVal != oldVal) { Add(field, oldVal, dtoVal); return true; }
            return false;
        }

        bool DiffEnum<T>(string field, T dtoVal, T oldVal) where T : struct, Enum
        {
            if (!EqualityComparer<T>.Default.Equals(dtoVal, oldVal))
            { Add(field, oldVal, dtoVal); return true; }
            return false;
        }

        bool DiffDouble(string field, double dtoVal, double oldVal)
        {
            if (Math.Abs(dtoVal - oldVal) > DoubleEps)
            { Add(field, oldVal, dtoVal); return true; }
            return false;
        }

        bool DiffDecimal(string field, decimal dtoVal, decimal oldVal)
        {
            if (Math.Abs(dtoVal - oldVal) > DecimalEps)
            { Add(field, oldVal, dtoVal); return true; }
            return false;
        }

        // Compare each field
        DiffStr(nameof(after.Title), after.Title, before.Title);
        DiffStr(nameof(after.Description), after.Description, before.Description);
        DiffStr(nameof(after.Street), after.Street, before.Street);
        DiffStr(nameof(after.City), after.City, before.City);
        DiffStr(nameof(after.State), after.State, before.State);

        DiffInt(nameof(after.Postcode), after.Postcode, before.Postcode);

        DiffDecimal(nameof(after.Longitude), after.Longitude, before.Longitude);
        DiffDecimal(nameof(after.Latitude), after.Latitude, before.Latitude);

        DiffDouble(nameof(after.Price), after.Price, before.Price);
        DiffInt(nameof(after.Bedrooms), after.Bedrooms, before.Bedrooms);
        DiffInt(nameof(after.Bathrooms), after.Bathrooms, before.Bathrooms);
        DiffInt(nameof(after.Garages), after.Garages, before.Garages);
        DiffDouble(nameof(after.FloorArea), after.FloorArea, before.FloorArea);
        DiffEnum(nameof(after.PropertyType), after.PropertyType, before.PropertyType);
        DiffEnum(nameof(after.SaleCategory), after.SaleCategory, before.SaleCategory);
        DiffEnum(nameof(after.ListcaseStatus), after.ListcaseStatus, before.ListcaseStatus);

        return changes;

    }



    private static string? ToInv(object? v)
    {
        if (v is null) return null;
        return v switch
        {
            DateTime dt => dt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
            decimal dec => dec.ToString("G29", CultureInfo.InvariantCulture),
            double d => d.ToString("R", CultureInfo.InvariantCulture),
            float f => f.ToString("R", CultureInfo.InvariantCulture),
            _ => Convert.ToString(v, CultureInfo.InvariantCulture)
        };
    }





}