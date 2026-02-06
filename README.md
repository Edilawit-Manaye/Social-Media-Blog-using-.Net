# G6 Social Blog Backend 🚀

An enterprise-grade social media backend built with **.NET 8** and **Clean Architecture**. This project demonstrates a scalable, secure, and modern approach to building web APIs.

## 🔥 Key Features

- **Clean Architecture**: Strict separation of concerns (Domain, Application, Infrastructure, API).
- **Social Interactions**: Create blogs, **Like** posts, and **Update** content dynamically.
- **Secure Auth**: Stateless authentication using **JWT** and **BCrypt**.
- **AI Integration**: Integrated AI service structure for generative content features.
- **NoSQL Performance**: Powered by **MongoDB** for flexible and fast data storage.

## 🛠️ Tech Stack

- **Framework**: .NET 8 Web API
- **Database**: MongoDB (Official Driver)
- **Documentation**: Swagger/OpenAPI
- **Testing**: Unit Testable Design (Dependency Injection)

## 🚀 Getting Started

1.  **Prerequisites**:
    *   .NET 8 SDK
    *   MongoDB running locally on `mongodb://localhost:27017`

2.  **Run the Application**:
    ```bash
    dotnet restore
    dotnet run --project G6Blog.Api
    ```

3.  **Explore API**:
    *   Navigate to `http://localhost:5000/swagger` (or port indicated in console) to test endpoints.

## 📂 Project Structure

```
├── G6Blog.Domain          # Core Entities & Interfaces (Zero Dependencies)
├── G6Blog.Application     # Business Logic & DTOs
├── G6Blog.Infrastructure  # DB, AI, & Auth Implementations
└── G6Blog.Api             # REST Controllers & Entry Point
```
