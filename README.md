# E-Store - ASP.NET Core E-Commerce Application

A modern, full-featured e-commerce web application built with ASP.NET Core Razor Pages, featuring AI-powered chat assistance, Redis caching, and comprehensive product management.

## 🚀 Features

### Product Management
- **CRUD Operations**: Complete Create, Read, Update, and Delete functionality for products
- **Product Catalog**: Browse products with images, descriptions, and pricing
- **Inventory Tracking**: Real-time stock quantity management
- **Product Details**: Detailed product pages with comprehensive information

### Shopping Cart
- **Session-based Cart**: Persistent shopping cart functionality
- **Cart Management**: Add, remove, and update items in cart
- **Real-time Updates**: Dynamic cart updates without page reload

### AI-Powered Chat Assistant
- **RAG Integration**: Retrieval-Augmented Generation for product-specific queries
- **Ollama Integration**: Local LLM powered by Ollama (DeepSeek v3.1)
- **Product Recommendations**: Intelligent product suggestions based on conversation context
- **Natural Language Queries**: Ask questions about products in natural language

### Performance & Caching
- **Redis Caching**: Distributed caching for improved performance
- **Cache Service**: Abstracted caching layer with pattern-based operations
- **Fallback Support**: In-memory cache fallback when Redis is unavailable

### Additional Features
- **Responsive Design**: Mobile-friendly UI with Bootstrap
- **Database Seeding**: Automatic data seeding for development
- **Entity Framework Core**: Code-first approach with migrations
- **Repository Pattern**: Clean architecture with Unit of Work pattern
- **AutoMapper**: Efficient object-to-object mapping

## 🛠️ Technologies

- **Framework**: ASP.NET Core 9.0 (Razor Pages)
- **Database**: Microsoft SQL Server with Entity Framework Core 9.0
- **Caching**: Redis (StackExchange.Redis)
- **AI/ML**: Ollama (DeepSeek v3.1 model)
- **Mapping**: AutoMapper 12.0
- **Testing**: xUnit, Moq, FluentAssertions
- **Performance Testing**: Apache JMeter
- **Frontend**: HTML5, CSS3, JavaScript, Bootstrap

## 📋 Prerequisites

Before running this application, ensure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (LocalDB or Express)
- [Redis](https://redis.io/download) (optional, falls back to in-memory cache)
- [Ollama](https://ollama.ai/) with DeepSeek v3.1 model (optional, for chat features)

### Installing Ollama Model (Optional)
```bash
ollama pull deepseek-v3.1:671b-cloud
```

## 🔧 Installation

1. **Clone the repository**
```bash
git clone <repository-url>
cd TP1
```

2. **Restore dependencies**
```bash
cd TP1
dotnet restore
```

3. **Configure the database connection**

Edit `appsettings.json` or `appsettings.Development.json` to match your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\MSSQLSERVER02;Database=TP1;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379,abortConnect=false,connectRetry=3,connectTimeout=5000"
  }
}
```

4. **Apply database migrations**
```bash
dotnet ef database update
```

5. **Run the application**
```bash
dotnet run
```

The application will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## ⚙️ Configuration

### Application Settings

The application can be configured through `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL Server connection string",
    "Redis": "Your Redis connection string"
  },
  "Redis": {
    "InstanceName": "E-Store:",
    "DefaultExpiration": "01:00:00"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "deepseek-v3.1:671b-cloud"
  }
}
```

### Optional Services

- **Redis**: If Redis is not available, the application will automatically fall back to in-memory caching
- **Ollama**: If Ollama is not running, the chat feature will not be available, but the rest of the application will function normally

## 📖 Usage

### Managing Products

1. Navigate to `/Products` to view all products
2. Click "Create New" to add a product
3. Use "Edit" or "Delete" to manage existing products
4. Click on a product to view detailed information

### Shopping Cart

1. Browse products and click "Add to Cart"
2. Navigate to `/Cart` to view your cart
3. Adjust quantities or remove items as needed

### AI Chat Assistant

1. Navigate to `/Chat`
2. Ask questions about products (e.g., "What laptops do you have?")
3. The AI assistant uses RAG to provide accurate product information
4. Get personalized recommendations based on your queries

### Promotions

1. Navigate to `/Promotions` to view current deals and special offers

## 📁 Project Structure

```
TP1/
├── DataLayer/              # Data access layer
│   ├── Interfaces/         # Repository interfaces
│   ├── Repositories/       # Repository implementations
│   ├── UnitOfWork/         # Unit of Work pattern
│   ├── DBContext.cs        # Entity Framework context
│   └── DataSeeder.cs       # Database seeding logic
├── DTO/                    # Data Transfer Objects
├── Models/                 # Domain models
├── Services/               # Business logic services
│   ├── CacheService.cs     # Redis caching service
│   ├── CartService.cs      # Shopping cart service
│   ├── ProductService.cs   # Product management service
│   ├── OllamaService.cs    # AI chat service
│   └── RAGService.cs       # Retrieval-Augmented Generation
├── Pages/                  # Razor Pages
│   ├── Products/           # Product management pages
│   ├── Cart/               # Shopping cart pages
│   ├── Chat/               # AI chat interface
│   └── Shared/             # Shared layout components
├── Helpers/                # Helper classes
│   └── AutoMapperProfile.cs
├── Migrations/             # EF Core migrations
├── wwwroot/                # Static files (CSS, JS, images)
└── Program.cs              # Application entry point

TP1.Tests/                  # Unit tests
├── Repositories/           # Repository tests
├── Services/               # Service tests
└── Pages/                  # Page model tests
```

## 🧪 Testing

The project includes comprehensive unit tests using xUnit, Moq, and FluentAssertions.

### Running Tests

```bash
cd TP1.Tests
dotnet test
```

### Test Coverage

- **Repository Tests**: Data access layer validation
- **Service Tests**: Business logic verification
  - CacheService tests
  - ProductService tests
- **Page Model Tests**: Razor Page logic testing

## 📊 Performance Testing

The project includes JMeter test plans for performance testing:

```bash
jmeter -n -t EStore_Perf_Test.jmx -l results.jtl
```

## 🏗️ Architecture

### Design Patterns

- **Repository Pattern**: Abstracts data access logic
- **Unit of Work**: Manages transactions across repositories
- **Dependency Injection**: Loose coupling and testability
- **Service Layer**: Separates business logic from presentation
- **DTO Pattern**: Data transfer between layers

### Key Technologies

- **Entity Framework Core**: ORM for database operations
- **AutoMapper**: Object-to-object mapping
- **Redis**: Distributed caching
- **RAG**: Retrieval-Augmented Generation for AI chat

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is part of an academic assignment for ILISI2 at FSTM.

## 👨‍💻 Development

### Adding New Migrations

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Development Environment

The application supports development-specific settings in `appsettings.Development.json`. Debug mode includes:
- Detailed error pages
- Database query logging
- Automatic data seeding

## 🔍 Troubleshooting

### Database Connection Issues
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database exists or run `dotnet ef database update`

### Redis Connection Issues
- Verify Redis is running: `redis-cli ping`
- Application will fall back to in-memory cache if Redis is unavailable

### Ollama Chat Issues
- Ensure Ollama is running: check `http://localhost:11434`
- Verify the model is installed: `ollama list`
- Pull the model if needed: `ollama pull deepseek-v3.1:671b-cloud`

## 📞 Support

For questions or issues, please open an issue in the repository.

---

**Built with ❤️ using ASP.NET Core**

