Write-Host "Testing Ollama Connection..." -ForegroundColor Yellow
Write-Host ""

# Test 1: Check if Ollama is running
Write-Host "1. Checking if Ollama is running..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:11434/api/tags" -Method Get -TimeoutSec 5
    Write-Host "   ✓ Ollama is running!" -ForegroundColor Green
    
    # Parse and display available models
    $models = ($response.Content | ConvertFrom-Json).models
    Write-Host "   Available models:" -ForegroundColor White
    foreach ($model in $models) {
        Write-Host "   - $($model.name)" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ✗ Ollama is NOT running!" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "   To start Ollama, run: ollama serve" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Test 2: Test the specific model
Write-Host "2. Testing deepseek-v3.1:671b-cloud model..." -ForegroundColor Cyan
$testPrompt = @{
    model = "deepseek-v3.1:671b-cloud"
    prompt = "Say 'Hello' in one word."
    stream = $false
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:11434/api/generate" `
        -Method Post `
        -Body $testPrompt `
        -ContentType "application/json" `
        -TimeoutSec 30
    
    $result = $response.Content | ConvertFrom-Json
    Write-Host "   ✓ Model responded successfully!" -ForegroundColor Green
    Write-Host "   Response: $($result.response)" -ForegroundColor White
} catch {
    Write-Host "   ✗ Model test failed!" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Message -like "*404*") {
        Write-Host ""
        Write-Host "   Model not found! Available options:" -ForegroundColor Yellow
        Write-Host "   1. Pull the model: ollama pull deepseek-v3.1:671b-cloud" -ForegroundColor Yellow
        Write-Host "   2. Or use a different model in appsettings.json" -ForegroundColor Yellow
    }
    exit 1
}

Write-Host ""
Write-Host "✓ All tests passed! Your Ollama setup is working correctly." -ForegroundColor Green
Write-Host ""
Write-Host "Now restart your application:" -ForegroundColor Yellow
Write-Host "  1. Stop the running app (Ctrl+C)" -ForegroundColor White
Write-Host "  2. Run: dotnet run" -ForegroundColor White

