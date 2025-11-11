#!/bin/bash

echo "🚀 Aggressive Rate Limit Test"
echo "=============================="
echo ""

# Test với 20 parallel requests cùng lúc
echo "📊 Sending 20 PARALLEL requests to /auth/login..."
echo "Rate limit: 10 requests/minute"
echo ""

counter_200=0
counter_400=0
counter_401=0
counter_429=0

for i in {1..20}; do
    (
        http_code=$(curl -X POST -s -o /dev/null -w "%{http_code}" \
            -H "Content-Type: application/json" \
            -d '{"username":"testuser","email":"test@test.com","password":"Test123!@#"}' \
            http://localhost:5050/auth/login)
        
        case $http_code in
            200) echo "✅ Request $i: HTTP 200 - Success" ;;
            400) echo "⚠️  Request $i: HTTP 400 - Bad Request" ;;
            401) echo "🔒 Request $i: HTTP 401 - Unauthorized" ;;
            429) echo "🛡️  Request $i: HTTP 429 - RATE LIMITED!" ;;
            *) echo "❓ Request $i: HTTP $http_code" ;;
        esac
    ) &
done

wait

echo ""
echo "=============================="
echo ""
echo "Testing with rapid sequential requests (no delay)..."

for i in {1..15}; do
    http_code=$(curl -X POST -s -o /dev/null -w "%{http_code}" \
        -H "Content-Type: application/json" \
        -d '{"username":"testuser","email":"test@test.com","password":"Test123!@#"}' \
        http://localhost:5050/auth/login)
    
    case $http_code in
        200) echo "✅ Request $i: HTTP 200" ;;
        400) echo "⚠️  Request $i: HTTP 400" ;;
        401) echo "🔒 Request $i: HTTP 401" ;;
        429) echo "🛡️  Request $i: HTTP 429 - RATE LIMITED!" ;;
        *) echo "❓ Request $i: HTTP $http_code" ;;
    esac
done

echo ""
echo "=============================="
echo "If you see HTTP 429 responses, Rate Limiting is WORKING! ✅"
echo "If you don't see 429, the rate limit may not be configured correctly."
