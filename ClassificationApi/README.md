# Classification API

A .NET Web API for stool classification using machine learning models.

## Features

- Stool classification using ONNX models
- Web frontend for image upload and classification
- Support for both embedded resources and ByteShelf storage
- EasyReasy resource management
- Environment variable validation

## Setup

### Prerequisites

- .NET 8.0
- ByteShelf account and API key (for model storage)

### Configuration

#### ByteShelf Setup

The application uses ByteShelf for storing the ML model. You need to:

1. **Get ByteShelf API Key**: Obtain an API key from your ByteShelf account
2. **Upload Model**: Upload your ONNX model to ByteShelf with the path `StoolClassification/kf_stool_classification_628.onnx`
3. **Configure Environment**: Set up the following environment variables:

**Required Environment Variables:**
```bash
BYTESHELF_API_KEY=your-api-key-here
BYTESHELF_BASE_URL=https://api.byteshelf.com
```

**Optional Environment Variables:**
```bash
BYTESHELF_CACHE_DIR=ByteShelfCache
```

The application validates environment variables at startup:
- API keys must be at least 20 characters long
- Base URLs must be at least 10 characters long
- Clear error messages if any variables are missing or invalid

> **Note:** For even better environment variable management, consider using `EasyReasy.EnvironmentVariables` package which provides type-safe environment variable access with compile-time validation.

#### ByteShelf File Structure

Your ByteShelf tenant should have the following structure:
```
StoolClassification/
  └── kf_stool_classification_628.onnx
```

The model file should be uploaded to a subtenant named "StoolClassification" with the display name "kf_stool_classification_628.onnx".

### Running the Application

1. **Set up ByteShelf configuration** (see above)
2. **Build the project:**
   ```bash
   dotnet build
   ```
3. **Run the application:**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001` (or your configured port).

### Testing

Run the tests with:
```bash
dotnet test
```

**Note:** Some tests require ByteShelf configuration to run. If ByteShelf is not configured, those tests will be skipped.

## API Endpoints

- `GET /` - Web frontend for image upload
- `GET /ping` - Health check
- `POST /predict` - Classify an uploaded image

## Resource Management

The application uses EasyReasy for resource management:

- **Models**: Stored in ByteShelf for easy updates and versioning
- **Frontend**: Embedded in the application for fast loading

## Environment Variable Management

The application validates environment variables at startup for better reliability:

- **Startup Validation**: All required environment variables are validated at startup
- **Length Validation**: API keys and URLs must meet minimum length requirements
- **Clear Error Messages**: Detailed feedback about missing or invalid variables

### Future Enhancement

For even better environment variable management, you can add the `EasyReasy.EnvironmentVariables` package:

```xml
<PackageReference Include="EasyReasy.EnvironmentVariables" Version="1.0.0" />
```

This would provide:
- Type-safe environment variable access
- Compile-time validation
- IntelliSense support
- Static analysis for finding all environment variable usages

## Caching

ByteShelf resources are cached locally for improved performance. The cache directory can be configured via the `BYTESHELF_CACHE_DIR` environment variable. 