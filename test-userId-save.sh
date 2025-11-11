#!/bin/bash

echo "🧪 Testing UserId Save Feature"
echo "================================"

# API Gateway URL
API_URL="http://localhost:5050"

# Step 1: Register a new user
echo ""
echo "📝 Step 1: Registering new user..."
TIMESTAMP=$(date +%s)
USERNAME="testuser-$TIMESTAMP"
EMAIL="test-userid-$TIMESTAMP@example.com"

REGISTER_RESPONSE=$(curl -s -X POST "$API_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"$EMAIL\",
    \"password\": \"Test@123\",
    \"username\": \"$USERNAME\"
  }")

echo "Register Response:"
echo "$REGISTER_RESPONSE" | jq '.'

# Step 2: Login to get JWT token
echo ""
echo "🔐 Step 2: Logging in..."
LOGIN_RESPONSE=$(curl -s -X POST "$API_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d "{
    \"username\": \"$USERNAME\",
    \"email\": \"$EMAIL\",
    \"password\": \"Test@123\"
  }")

# Extract token
TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.token // .accessToken // .access_token // empty')

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
    echo "❌ Failed to get token. Login response:"
    echo "$LOGIN_RESPONSE" | jq '.'
    exit 1
fi

echo "✅ Got token: ${TOKEN:0:50}..."

# Extract userId from token (decode JWT)
echo ""
echo "🔍 Step 3: Decoding JWT to get userId..."
# Decode JWT payload (base64 decode the middle part)
PAYLOAD=$(echo "$TOKEN" | cut -d '.' -f 2)
# Add padding if needed
PADDING=$((4 - ${#PAYLOAD} % 4))
if [ $PADDING -ne 4 ]; then
    PAYLOAD="${PAYLOAD}$(printf '=%.0s' $(seq 1 $PADDING))"
fi
USER_DATA=$(echo "$PAYLOAD" | base64 -d 2>/dev/null)
USER_ID=$(echo "$USER_DATA" | jq -r '.sub // .userId // empty')

echo "User Data from JWT:"
echo "$USER_DATA" | jq '.'
echo ""
echo "✅ UserId: $USER_ID"

# Step 4: Create a short URL
echo ""
echo "🔗 Step 4: Creating short URL with userId..."
CREATE_URL_RESPONSE=$(curl -s -X POST "$API_URL/url/create" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "originalUrl": "https://example.com/test-'$(date +%s)'"
  }')

echo "Create URL Response:"
echo "$CREATE_URL_RESPONSE" | jq '.'

# Extract shortCode from shortUrl
SHORT_URL=$(echo "$CREATE_URL_RESPONSE" | jq -r '.shortUrl // empty')
SHORT_CODE=$(echo "$SHORT_URL" | awk -F'/' '{print $NF}')

if [ -z "$SHORT_CODE" ] || [ "$SHORT_CODE" == "null" ]; then
    echo "❌ Failed to create URL"
    exit 1
fi

echo "✅ Created short URL with code: $SHORT_CODE"

# Step 5: Verify in database
echo ""
echo "🔍 Step 5: Checking database for userId..."
echo "Please check in database:"
echo "  SELECT \"Id\", \"ShortCode\", \"OriginalUrl\", \"UserId\" FROM \"ShortUrls\" WHERE \"ShortCode\" = '$SHORT_CODE';"
echo ""

# Query database (if psql is available)
if command -v psql &> /dev/null; then
    echo "📊 Querying database..."
    PGPASSWORD="urlShortener123" psql -h url-shortener-instance.cpicai0qavde.ap-southeast-1.rds.amazonaws.com \
        -U postgres -d url_db \
        -c "SELECT \"Id\", \"ShortCode\", \"OriginalUrl\", \"UserId\", \"CreatedAt\" FROM \"ShortUrls\" WHERE \"ShortCode\" = '$SHORT_CODE';"
    
    # Check if userId matches
    DB_USER_ID=$(PGPASSWORD="urlShortener123" psql -h url-shortener-instance.cpicai0qavde.ap-southeast-1.rds.amazonaws.com \
        -U postgres -d url_db -t \
        -c "SELECT \"UserId\" FROM \"ShortUrls\" WHERE \"ShortCode\" = '$SHORT_CODE';")
    
    DB_USER_ID=$(echo "$DB_USER_ID" | tr -d ' \n')
    
    echo ""
    echo "Expected UserId: $USER_ID"
    echo "Database UserId: $DB_USER_ID"
    
    if [ "$USER_ID" == "$DB_USER_ID" ]; then
        echo "✅ ✅ ✅ SUCCESS! UserId is correctly saved in database!"
    else
        echo "❌ FAILED! UserId mismatch"
        echo "   Expected: $USER_ID"
        echo "   Got:      $DB_USER_ID"
    fi
else
    echo "⚠️  psql not installed. Please check database manually."
fi

echo ""
echo "================================"
echo "🎉 Test completed!"
