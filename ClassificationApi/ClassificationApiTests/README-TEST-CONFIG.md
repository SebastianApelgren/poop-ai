# Test Configuration Setup

This file explains how to set up ByteShelf credentials for running tests.

## Test Configuration File

The tests use a `test-config.env` file to load ByteShelf credentials. This file is gitignored to prevent committing sensitive information.

### Setup Instructions

1. **Copy the template file**:
   ```bash
   cp test-config.env.template test-config.env
   ```

2. **Edit the test configuration file**:
   ```
   ClassificationApi/ClassificationApiTests/test-config.env
   ```

3. **Replace the placeholder values** with your actual ByteShelf credentials:
   ```
   BYTESHELF_API_KEY=your-actual-api-key-here
   BYTESHELF_BASE_URL=https://api.byteshelf.com
   BYTESHELF_CACHE_DIR=TestByteShelfCache
   ```

4. **Run the tests**:
   ```bash
   dotnet test
   ```

### Test Categories

The test suite includes several categories of tests:

#### Unit Tests (No ByteShelf Required)
- `StoolClassificationServiceTests.cs` - Tests service logic with mocked dependencies
- Basic model validation and error handling

#### Integration Tests (ByteShelf Required)
- `ResourceHelperTests.cs` - Tests resource loading and ByteShelf integration
- `ModelInferenceTests.cs` - **NEW**: Tests actual model inference and classification

### Model Inference Tests

The new `ModelInferenceTests.cs` file includes comprehensive tests for:

- **Model Loading**: Verifies the ONNX model can be loaded from ByteShelf
- **Model Metadata**: Validates input/output names and shapes
- **Inference Execution**: Tests that the model can run inference with valid inputs
- **Image Classification**: Tests the full classification pipeline with various image types
- **Error Handling**: Tests behavior with invalid inputs
- **Consistency**: Ensures the same input produces the same output
- **Class Coverage**: Verifies the model can predict different stool types

### File Format

The `test-config.env` file uses standard environment variable format:
```
VARIABLE_NAME=value
# Comments are supported
ANOTHER_VARIABLE=another_value
```

### Security Notes

- The `test-config.env` file is gitignored and will not be committed to version control
- Never commit real API keys to version control
- Tests will skip ByteShelf-dependent tests if no valid credentials are found
- Tests will show "Inconclusive" status if ByteShelf is not configured

### Troubleshooting

If tests are being skipped:
1. Check that `test-config.env` exists in the test project directory
2. Verify that `BYTESHELF_API_KEY` is set to a real API key (not the placeholder)
3. Ensure the API key is at least 20 characters long
4. Check that the ByteShelf URL is correct

### Running Specific Test Categories

To run only unit tests (no ByteShelf required):
```bash
dotnet test --filter "TestCategory!=Integration"
```

To run only integration tests (requires ByteShelf):
```bash
dotnet test --filter "TestCategory=Integration"
```

To run all tests:
```bash
dotnet test
``` 