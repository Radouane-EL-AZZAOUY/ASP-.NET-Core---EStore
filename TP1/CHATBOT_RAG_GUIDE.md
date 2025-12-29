# AI Shopping Assistant - RAG-Powered Chatbot Guide

## Overview

Your E-Store application now includes an intelligent AI Shopping Assistant powered by Ollama LLM with RAG (Retrieval-Augmented Generation). The chatbot can help customers find products, provide recommendations, and answer questions based on your actual product inventory.

## Features

✅ **Product-Aware Conversations**: The chatbot retrieves relevant products from your database and includes them in the context  
✅ **Smart Product Search**: Uses keyword matching and relevance scoring to find the best products  
✅ **Visual Product Cards**: Displays recommended products as interactive cards with images and prices  
✅ **Real-Time Availability**: Mentions stock status and suggests alternatives for out-of-stock items  
✅ **Price-Sensitive Recommendations**: Understands budget constraints ("cheap", "affordable", "premium")  
✅ **Conversation History**: Maintains context across multiple messages  

## Setup Instructions

### 1. Install Ollama

If you haven't installed Ollama yet:

**Windows:**
1. Download Ollama from https://ollama.ai/download
2. Run the installer
3. Ollama will start automatically as a service

**Verify Installation:**
```bash
ollama --version
```

### 2. Pull a Language Model

Download a model to use (llama2 is configured by default):

```bash
# Default model (recommended for most users)
ollama pull llama2

# Alternative models (if you have more RAM/GPU):
ollama pull llama3
ollama pull mistral
ollama pull phi
```

### 3. Start Ollama Service

Ollama should start automatically, but if needed:

```bash
# Windows - Ollama runs as a service automatically
# Or manually start:
ollama serve
```

Verify it's running by visiting: http://localhost:11434

### 4. Configure the Application

The application is already configured in `appsettings.json`:

```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "llama2"
  }
}
```

**To use a different model**, change the `Model` value to match the model you pulled (e.g., "llama3", "mistral", "phi").

### 5. Run Your Application

```bash
cd TP1
dotnet run
```

Navigate to: **http://localhost:5xxx/Chat**

## How It Works (RAG Architecture)

### 1. User Query Processing
When a user asks a question, the system:
- Analyzes the query to determine if it's product-related
- Extracts keywords and search terms

### 2. Retrieval Phase (The "R" in RAG)
The `RAGService` retrieves relevant products by:
- Searching the database using the `ProductService`
- Scoring products by relevance (title matches, description matches, price, stock status)
- Returning the top 3-5 most relevant products

### 3. Context Augmentation (The "A" in RAG)
Product information is formatted as context:
```
=== Available Products in Our Store ===

Product: MacBook Pro 14"
Description: Professional laptop with M3 chip
Price: $1,999.00
Availability: In Stock
Product ID: 123

...
```

### 4. Generation Phase (The "G" in RAG)
The `OllamaService`:
- Combines the system prompt, product context, and user query
- Sends the augmented prompt to Ollama
- Returns an informed response based on actual data

## Usage Examples

### Basic Product Queries
- "Show me laptops"
- "What phones do you have?"
- "Do you have any AirPods?"

### Price-Based Queries
- "Show me laptops under $1000"
- "What's your cheapest product?"
- "Recommend an expensive premium laptop"

### Recommendation Requests
- "I need a laptop for programming"
- "Best phone for photography"
- "Something for gaming"

### Availability Queries
- "Is the MacBook Pro in stock?"
- "What products are available right now?"

## Architecture Overview

```
User Query
    ↓
Chat Page (Index.cshtml)
    ↓
IndexModel (Index.cshtml.cs)
    ↓
OllamaService ←→ RAGService ←→ ProductService ←→ Database
    ↓                    ↓
Ollama API          Product Context
    ↓                    ↓
    └─────→ AI Response ←──┘
```

## Key Components

### Services

1. **IOllamaService / OllamaService**
   - Manages communication with Ollama API
   - Builds system prompts with product context
   - Handles error cases and timeouts

2. **IRAGService / RAGService**
   - Retrieves relevant products from database
   - Scores products by relevance
   - Formats product context for LLM

3. **IProductService / ProductService**
   - Existing service for product operations
   - Used by RAG to fetch products
   - Includes caching for performance

### DTOs

- **ChatMessageDTO**: User message + conversation history
- **ChatResponseDTO**: AI response + relevant product list

### Pages

- **Chat/Index.cshtml**: Chat UI with product cards
- **Chat/Index.cshtml.cs**: Page model handling requests

## Configuration Options

### Changing the Model

Edit `appsettings.json`:
```json
{
  "Ollama": {
    "Model": "mistral"  // or llama3, phi, etc.
  }
}
```

### Adjusting Response Quality

In `OllamaService.cs`, modify the options:
```csharp
options = new
{
    temperature = 0.7,  // Lower = more focused, Higher = more creative
    top_p = 0.9,        // Nucleus sampling threshold
    top_k = 40          // Top-K sampling value
}
```

### Limiting Product Results

In `RAGService.cs`, change the default:
```csharp
public async Task<string> GetProductContextAsync(string userQuery, int maxProducts = 5)
```

## Performance Considerations

### Response Times
- First query: 5-15 seconds (model loading)
- Subsequent queries: 2-5 seconds
- Depends on: Model size, hardware, query complexity

### Optimization Tips
1. **Use smaller models** for faster responses (phi, mistral)
2. **Cache product searches** (already implemented)
3. **Limit product context** to top 3-5 items
4. **Run Ollama on GPU** if available

## Troubleshooting

### "Ollama is not running"
- Check if Ollama service is running: `ollama serve`
- Verify the URL: http://localhost:11434
- Check firewall settings

### Slow Responses
- First query is always slower (model loading)
- Try a smaller model like "phi"
- Check system resources (RAM/CPU usage)

### "No products found"
- Verify products exist in database
- Check product search functionality
- Review `ProductKeywords` in `RAGService.cs`

### Build Errors
- Run `dotnet restore`
- Ensure all services are registered in `Program.cs`
- Check that all files are created correctly

## Testing

### Manual Testing
1. Visit `/Chat` page
2. Check status badge (should be "Online")
3. Try sample queries:
   - "Show me laptops"
   - "What's the cheapest product?"
   - "Recommend something"

### Integration Tests
The existing 38 tests still pass. Future enhancements could include:
- RAGService unit tests
- OllamaService mock tests
- Chat page integration tests

## Future Enhancements

Potential improvements:
- **Vector Embeddings**: Use semantic search instead of keyword matching
- **Conversation Memory**: Store chat history in Redis
- **Multi-language Support**: Translate queries and responses
- **Voice Input**: Add speech-to-text capability
- **Product Comparisons**: Compare multiple products side-by-side
- **Order Assistance**: Help users complete purchases
- **Feedback Loop**: Learn from user interactions

## Security Considerations

- API is server-side only (no exposed Ollama endpoint)
- Input validation on chat messages
- Rate limiting recommended for production
- Consider authentication for chat access

## Resources

- **Ollama Documentation**: https://github.com/ollama/ollama
- **Available Models**: https://ollama.ai/library
- **RAG Concepts**: https://en.wikipedia.org/wiki/Retrieval-augmented_generation

## Support

For issues or questions:
1. Check Ollama logs: `ollama logs`
2. Review application logs in console
3. Verify configuration in `appsettings.json`
4. Ensure all services are registered in `Program.cs`

---

**Congratulations!** Your E-Store now has an intelligent AI assistant that can help customers find products using natural language. 🎉

