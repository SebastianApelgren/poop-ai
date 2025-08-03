# Test Configuration Setup

This file explains how to set up ByteShelf credentials for running tests.

## Test Configuration File

The tests use a `test-config.env` file to load ByteShelf credentials. This file is gitignored to prevent committing sensitive information.

### Setup Instructions

1. **Edit the test configuration file**:
   ```
   ClassificationApi/ClassificationApiTests/test-config.env
   ```

2. **Replace the placeholder values** with your actual ByteShelf credentials:
   ```
   BYTESHELF_API_KEY=your-actual-api-key-here
   BYTESHELF_BASE_URL=https://api.byteshelf.com
   BYTESHELF_CACHE_DIR=TestByteShelfCache
   ```

3. **Run the tests**:
   ```bash
   dotnet test
   ```

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
- The test will skip ByteShelf tests if no valid credentials are found
- Tests will show "Inconclusive" status if ByteShelf is not configured

### Troubleshooting

If tests are being skipped:
1. Check that `test-config.env` exists in the test project directory
2. Verify that `BYTESHELF_API_KEY` is set to a real API key (not the placeholder)
3. Ensure the API key is at least 20 characters long
4. Check that the ByteShelf URL is correct 