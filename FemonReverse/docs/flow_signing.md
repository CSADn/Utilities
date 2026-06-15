# FemonReverse - Flow CDN Signing Guide

## Bearer Token Acquisition
The `bearerToken` is fetched from a URL provided in Remote Config (`flow_bearer_json_url`).
- **Request**: HTTP GET with Chrome 125 User-Agent.
- **Extraction Priority**:
    1. JSON field `bearerToken`
    2. JSON field `token`
    3. JSON field `bearer`
    4. JSON field `access_token`
    5. Fallback: Any string longer than 10 characters in the JSON.
    6. Final Fallback: The raw response body (if not JSON and < 2000 chars).

## Signing Process (`SignChannel`)

### Step 1: CDN URL Construction
The tool takes the original channel URL and the `cdnGenBase` template.
- **Encoding**: The full original URL is passed through `Uri.EscapeDataString`.
- **Replacement**:
    - Replaces `$encodedPath` in the template.
    - Fallbacks: `{encoded_url}`, `{url}`, or simple concatenation with `?url=`.

### Step 2: Token Request
An HTTP GET is sent to the constructed CDN URL with the following headers:
- `Authorization`: `Bearer {bearerToken}`
- `Origin`: Defined in Remote Config (`flow_site_origin`).
- `Referer`: `{flow_site_origin}/`
- `User-Agent`: Chrome 125.

The response is expected to be a JSON containing the signed token in fields `url`, `token`, or `signedUrl`.

### Step 3: Final URL Assembly
- If the signed token is a full URL $\rightarrow$ Used directly.
- If the signed token is a fragment $\rightarrow$ Appended to the original URL as `?cdntoken={signedToken}` (or `&cdntoken=` if query params exist).

### Step 4: Resolution
The final URL is requested via HTTP GET. The tool follows redirects to resolve the actual streaming source and removes the query string from the final result.
