#!/bin/bash

echo "🛡️ Testing Rate Limiting on API Gateway"
echo "========================================"
echo ""

# Test 1: General endpoint (10 req/second limit)
echo "📊 Test 1: Testing /auth/login (limit: 10 requests/minute)"
echo "Sending 15 requests rapidly..."
echo ""

for i in {1..15}; do
    response=$(curl -X POST -s -w "\n%{http_code}" \
        -H "Content-Type: application/json" \
        -d '{"username":"testuser","email":"test@test.com","password":"Test123!@#"}' \
        http://localhost:5050/auth/login 2>&1)
    
    http_code=$(echo "$response" | tail -1)
    
    if [ "$http_code" = "429" ]; then
        echo "✅ Request $i: HTTP 429 - Rate Limited! (Working as expected)"
    else
        echo "⚪ Request $i: HTTP $http_code"
    fi
done

echo ""
echo "========================================"
echo ""

# Test 2: Register endpoint (5 req/minute limit)
echo "📊 Test 2: Testing /auth/register (limit: 5 requests/minute)"
echo "Sending 8 requests..."
echo ""

for i in {1..8}; do
    response=$(curl -X POST -s -w "\n%{http_code}" \
        -H "Content-Type: application/json" \
        -d "{\"username\":\"user$i\",\"email\":\"test$i@test.com\",\"password\":\"Test123!@#\"}" \
        http://localhost:5050/auth/register 2>&1)
    
    http_code=$(echo "$response" | tail -1)
    
    if [ "$http_code" = "429" ]; then
        echo "✅ Request $i: HTTP 429 - Rate Limited! (Working as expected)"
    else
        echo "⚪ Request $i: HTTP $http_code"
    fi
    
    sleep 0.5
done

echo ""
echo "========================================"
echo "✅ Rate Limiting Test Complete!"
echo ""
echo "Expected behavior:"
echo "- First 10 requests to /auth/login should pass"
echo "- Requests 11-15 should get HTTP 429 (Too Many Requests)"
echo "- First 5 requests to /auth/register should pass"
echo "- Requests 6-8 should get HTTP 429"
echo ""
echo "If you see 429 responses, rate limiting is working! 🎉"
