# Zava Storefront - ASP.NET Core MVC

A simple e-commerce storefront application built with .NET 6 ASP.NET MVC.

## Features

- **Product Listing**: Browse a catalog of 8 sample products with images, descriptions, and prices
- **Shopping Cart**: Add products to cart with session-based storage
- **Cart Management**: View cart, update quantities, remove items
- **Checkout**: Simple checkout process that clears cart and shows success message
- **Responsive Design**: Mobile-friendly layout using Bootstrap 5
- **Chat (Phi-4)**: Conversational interface powered by the Phi-4 model deployed on Microsoft Azure AI Foundry

## Technology Stack

- .NET 6
- ASP.NET Core MVC
- Bootstrap 5
- Bootstrap Icons
- Session-based state management (no database)

## Project Structure

```
ZavaStorefront/
├── Controllers/
│   ├── HomeController.cs      # Products listing and add to cart
│   ├── CartController.cs       # Cart operations and checkout
│   └── ChatController.cs       # Chat page and Phi-4 message handling
├── Models/
│   ├── Product.cs              # Product model
│   └── CartItem.cs             # Cart item model
├── Services/
│   ├── ProductService.cs       # Static product data
│   ├── CartService.cs          # Session-based cart management
│   └── ChatService.cs          # Azure AI Foundry (Phi-4) client
├── Views/
│   ├── Home/
│   │   └── Index.cshtml        # Products listing page
│   ├── Cart/
│   │   ├── Index.cshtml        # Shopping cart page
│   │   └── CheckoutSuccess.cshtml  # Checkout success page
│   ├── Chat/
│   │   └── Index.cshtml        # Chat page (Phi-4 conversation)
│   └── Shared/
│       └── _Layout.cshtml      # Main layout with cart icon and Chat nav link
└── wwwroot/
    ├── css/
    │   └── site.css            # Custom styles
    └── images/
        └── products/           # Product images directory
```

## How to Run

1. Navigate to the project directory:
   ```bash
   cd ZavaStorefront
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser and navigate to:
   ```
   https://localhost:5001
   ```

## Product Images

The application includes 8 sample products. Product images are referenced from:
- `/wwwroot/images/products/`

If images are not found, the application automatically falls back to placeholder images from placeholder.com.

To add custom product images, place JPG files in `wwwroot/images/products/` with these names:
- headphones.jpg
- smartwatch.jpg
- speaker.jpg
- charger.jpg
- usb-hub.jpg
- keyboard.jpg
- mouse.jpg
- webcam.jpg

## Sample Products

1. Wireless Bluetooth Headphones - $89.99
2. Smart Fitness Watch - $199.99
3. Portable Bluetooth Speaker - $49.99
4. Wireless Charging Pad - $29.99
5. USB-C Hub Adapter - $39.99
6. Mechanical Gaming Keyboard - $119.99
7. Ergonomic Wireless Mouse - $34.99
8. HD Webcam - $69.99

## Application Flow

1. **Landing Page**: Displays all products in a responsive grid
2. **Add to Cart**: Click "Buy" button to add products to cart
3. **View Cart**: Click cart icon (top right) to view cart contents
4. **Update Cart**: Modify quantities or remove items
5. **Checkout**: Click "Checkout" button to complete purchase
6. **Success**: View confirmation and return to products

## Session Management

- Cart data is stored in session
- Session timeout: 30 minutes
- No data persistence (cart clears when session expires)
- Cart is cleared after successful checkout

## Chat Feature (Phi-4 via Azure AI Foundry)

The **Chat** page at `/Chat` lets users send messages directly to the Phi-4 model deployed on Microsoft Azure AI Foundry. Responses are appended to an on-screen conversation history text area.

### Endpoint configuration

The chat feature reads its settings from `appsettings.json` (or environment variables / user secrets for local development):

| Key | Description | Example |
|-----|-------------|---------|
| `AzureAIFoundry:Endpoint` | Azure AI Services endpoint URL | `https://<account>.cognitiveservices.azure.com` |
| `AzureAIFoundry:ApiKey` | API key for authentication | `<your-api-key>` |
| `AzureAIFoundry:DeploymentName` | Deployment name of the Phi-4 model | `phi-4` |
| `AzureAIFoundry:ApiVersion` | Azure OpenAI API version to target | `2024-05-01-preview` |

**Never commit secrets to source control.** Use [.NET user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets){:target="_blank"} for local development:

```bash
dotnet user-secrets set "AzureAIFoundry:Endpoint" "https://<account>.cognitiveservices.azure.com"
dotnet user-secrets set "AzureAIFoundry:ApiKey"   "<your-api-key>"
```

Or set the equivalent environment variables (`AzureAIFoundry__Endpoint`, `AzureAIFoundry__ApiKey`) when deploying to Azure App Service.

### Page usage

1. Click **Chat** in the top navigation bar.
2. Type a message in the **Your message** input and click **Send**.
3. The reply from Phi-4 is appended to the **Conversation** text area.
4. Click **Clear** to reset the conversation history.

## Logging

The application includes structured logging for:
- Product page loads
- Adding products to cart
- Cart operations (update, remove)
- Checkout process

Logs are written to console during development.
