# Chatbot Troubleshooting Guide

## "Failed to get response. Please try again."

If you're getting this error every time, follow these steps:

### 1. Check if Ollama is Running

Open a new terminal and run:
```bash
# Check if Ollama is running
curl http://localhost:11434/api/tags

# Or navigate to this URL in your browser:
# http://localhost:11434
```

If you get a connection error:
```bash
# Start Ollama
ollama serve
```

### 2. Verify Model is Installed

```bash
# List installed models
ollama list

# If empty, pull a model:
ollama pull llama2
```

### 3. Check Application Logs

Look at your console output when running the app for errors like:
- "Error calling Ollama API"
- Connection timeout errors
- Service resolution errors

### 4. Test Ollama Directly

```bash
# Test Ollama directly
ollama run llama2 "Hello, how are you?"
```

If this works but the web app doesn't, the issue is with the integration.

### 5. Restart the Application

Stop your running application (Ctrl+C in the terminal) and restart it:

```bash
cd "d:\FSTM\ILISI2\.NET\TP1\TP1"
dotnet run
```

### 6. Check Browser Console

Open your browser's Developer Tools (F12) and check the Console tab for errors:
- Network errors
- JavaScript errors
- Failed fetch requests

### 7. Verify Configuration

Check `appsettings.json`:
```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "llama2"
  }
}
```

Make sure the model name matches what you have installed.

### 8. Common Issues

#### Issue: "Connection refused"
**Solution**: Ollama is not running. Start it with `ollama serve`

#### Issue: "Model not found"
**Solution**: Pull the model: `ollama pull llama2`

#### Issue: "Timeout"
**Solution**: 
- Your first query takes longer (model loading)
- Try a smaller model like `phi` or `mistral`
- Increase timeout in `OllamaService.cs`:
  ```csharp
  _httpClient.Timeout = TimeSpan.FromMinutes(5);
  ```

#### Issue: "Anti-forgery token validation failed"
**Solution**: The page should now have the token. Try refreshing the page.

#### Issue: "Circular dependency" error on startup
**Solution**: This has been fixed in the latest version. Make sure you're using the updated `Program.cs`

### 9. Enable Detailed Logging

Update `appsettings.json` to see more details:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

### 10. Test the RAG Service Separately

If Ollama is working but you're still having issues, test if products are being retrieved:

1. Add a breakpoint in `RAGService.GetRelevantProductsAsync`
2. Or add logging:
   ```csharp
   _logger.LogInformation("Retrieved {Count} products for query: {Query}", 
       products.Count(), query);
   ```

### Still Having Issues?

1. **Stop all running TP1 processes**:
   - Check Task Manager for any TP1.exe processes
   - Kill them if found

2. **Clean and rebuild**:
   ```bash
   dotnet clean
   dotnet build
   dotnet run
   ```

3. **Check firewall**: Make sure your firewall isn't blocking localhost:11434

4. **Try a different model**:
   ```bash
   ollama pull phi
   ```
   Then update `appsettings.json`:
   ```json
   "Ollama": { "Model": "phi" }
   ```

5. **Check the Network tab** in browser DevTools:
   - Does the request to `/Chat?handler=SendMessage` go through?
   - What's the response status code?
   - What's the response body?

### Quick Test Script

Create a file `test-ollama.ps1`:
```powershell
Write-Host "Testing Ollama Connection..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "http://localhost:11434/api/tags" -Method Get
    Write-Host "✓ Ollama is running!" -ForegroundColor Green
    Write-Host $response.Content
} catch {
    Write-Host "✗ Ollama is NOT running!" -ForegroundColor Red
    Write-Host "Start it with: ollama serve" -ForegroundColor Yellow
}
```

Run it with: `powershell .\test-ollama.ps1`

### Need More Help?

Check the application logs in the console for specific error messages and search for them in the documentation.

