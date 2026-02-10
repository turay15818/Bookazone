namespace Bookazone.Application.Common.Shared.Utils;

public static class SwaggerConfig
{
    public class SwaggerDocChild
    {
        public required string Slug { get; set; }
        public required string Title { get; set; }
        public required string Version { get; set; }
        public required string Description { get; set; }
        public required string Name { get; set; }
    }

    public static readonly List<SwaggerDocChild> SwaggerDocs =
    [
        new SwaggerDocChild
        {
            Slug = SwaggerDocName.WebVersion1.Slug, 
            Title = SwaggerDocName.WebVersion1.Title, 
            Version = SwaggerDocName.WebVersion1.Version, 
            Description = SwaggerDocName.WebVersion1.Description, 
            Name = SwaggerDocName.WebVersion1.Name
        },
        new SwaggerDocChild
        {
            Slug = SwaggerDocName.Otp.Slug, 
            Title = SwaggerDocName.Otp.Title, 
            Version = SwaggerDocName.Otp.Version, 
            Description = SwaggerDocName.Otp.Description, 
            Name = SwaggerDocName.Otp.Name
        },
        new SwaggerDocChild
        {
            Slug = SwaggerDocName.EncryptDecrypt.Slug, 
            Title = SwaggerDocName.EncryptDecrypt.Title, 
            Version = SwaggerDocName.EncryptDecrypt.Version, 
            Description = SwaggerDocName.EncryptDecrypt.Description, 
            Name = SwaggerDocName.EncryptDecrypt.Name
        },
        new SwaggerDocChild
        {
            Slug = SwaggerDocName.Booking.Slug,
            Title = SwaggerDocName.Booking.Title,
            Version = SwaggerDocName.Booking.Version,
            Description = SwaggerDocName.Booking.Description,
            Name = SwaggerDocName.Booking.Name
        },
        new SwaggerDocChild
        {
            Slug = SwaggerDocName.Events.Slug,
            Title = SwaggerDocName.Events.Title,
            Version = SwaggerDocName.Events.Version,
            Description = SwaggerDocName.Events.Description,
            Name = SwaggerDocName.Events.Name
        },
        new SwaggerDocChild
        {
            Slug = SwaggerDocName.Vehicles.Slug,
            Title = SwaggerDocName.Vehicles.Title,
            Version = SwaggerDocName.Vehicles.Version,
            Description = SwaggerDocName.Vehicles.Description,
            Name = SwaggerDocName.Vehicles.Name
        },
        new SwaggerDocChild
        {
            Slug = SwaggerDocName.Reviews.Slug,
            Title = SwaggerDocName.Reviews.Title,
            Version = SwaggerDocName.Reviews.Version,
            Description = SwaggerDocName.Reviews.Description,
            Name = SwaggerDocName.Reviews.Name
        },
    ];

    public static class SwaggerDocName
    {
        public static class WebVersion1
        {
            public const string Slug = "API Web Version 1";
            public const string Title = "API Bookazone v1";
            public const string Version = "v1";
            public const string Description = "Bookazone";
            public const string Name = "Web";
        }
        
        public static class EncryptDecrypt
        {
            public const string Slug = "API Encrypt & Decrypt v1";
            public const string Title = "API Bookazone v1 (Encrypt & Decrypt)";
            public const string Version = "v1";
            public const string Description = "";
            public const string Name = "EncryptDecrypt";
        }
        
        public static class Otp
        {
            public const string Slug = "API One Time Password (OTP) v1";
            public const string Title = "API One Time Password (OTP) v1";
            public const string Version = "v1";
            public const string Description = "";
            public const string Name = "OTP";
        }

        public static class Booking
        {
            public const string Slug = "API Booking v1";
            public const string Title = "API Bookazone Booking v1";
            public const string Version = "v1";
            public const string Description = "Booking discovery and customer booking endpoints.";
            public const string Name = "Booking";
        }

        public static class Events
        {
            public const string Slug = "API Events v1";
            public const string Title = "API Bookazone Events v1";
            public const string Version = "v1";
            public const string Description = "Event discovery and event management endpoints.";
            public const string Name = "Events";
        }

        public static class Vehicles
        {
            public const string Slug = "API Vehicles v1";
            public const string Title = "API Bookazone Vehicles v1";
            public const string Version = "v1";
            public const string Description = "Vehicle rental and transport endpoints.";
            public const string Name = "Vehicles";
        }

        public static class Reviews
        {
            public const string Slug = "API Reviews v1";
            public const string Title = "API Bookazone Reviews v1";
            public const string Version = "v1";
            public const string Description = "Review and rating endpoints.";
            public const string Name = "Reviews";
        }
    }
}
