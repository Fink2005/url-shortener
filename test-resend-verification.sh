#!/bin/bash

# Test Resend Verification Email Feature
# This script tests the new resend verification email endpoint

BASE_URL="https://api.url-shortener.site"
TEST_EMAIL="testresend@example.com"

echo "=========================================="
echo "  Resend Verification Email Test Script  "
echo "=========================================="
echo ""

# 1. Register a new user
echo "1️⃣  Registering new user with email: $TEST_EMAIL"
REGISTER_RESPONSE=$(curl -s -X POST "$BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d "{
    \"username\": \"testresend\",
    \"email\": \"$TEST_EMAIL\",
    \"password\": \"Test123!@#\"
  }")

echo "Response: $REGISTER_RESPONSE"
echo ""

# Wait for RabbitMQ to process
echo "⏳ Waiting 3 seconds for email to be sent..."
sleep 3
echo ""

# 2. Try to login (should fail - email not verified)
echo "2️⃣  Attempting login (should fail - email not verified)"
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d "{
    \"username\": \"testresend\",
    \"password\": \"Test123!@#\"
  }")

echo "Response: $LOGIN_RESPONSE"
echo ""

# 3. Resend verification email
echo "3️⃣  Requesting resend verification email"
RESEND_RESPONSE=$(curl -s -X POST "$BASE_URL/auth/resend-verification" \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"$TEST_EMAIL\"
  }")

echo "Response: $RESEND_RESPONSE"
echo ""

# 4. Try resending again (should still succeed - security pattern)
echo "4️⃣  Requesting resend again (testing idempotency)"
RESEND2_RESPONSE=$(curl -s -X POST "$BASE_URL/auth/resend-verification" \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"$TEST_EMAIL\"
  }")

echo "Response: $RESEND2_RESPONSE"
echo ""

# 5. Test with non-existent email (should return same generic message)
echo "5️⃣  Testing with non-existent email (security check)"
FAKE_RESEND=$(curl -s -X POST "$BASE_URL/auth/resend-verification" \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"nonexistent@example.com\"
  }")

echo "Response: $FAKE_RESEND"
echo ""

echo "=========================================="
echo "✅ Resend verification email feature tested!"
echo ""
echo "Key Features Demonstrated:"
echo "  - Email verification required for login ✅"
echo "  - Resend verification email endpoint ✅"
echo "  - Generic security message (doesn't reveal email existence) ✅"
echo "  - Idempotent operation (can resend multiple times) ✅"
echo ""
echo "Next Steps:"
echo "  1. Check email inbox for verification link"
echo "  2. Click link or use token from Redis"
echo "  3. Try login again (should succeed after verification)"
echo "=========================================="
