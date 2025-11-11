#!/bin/bash

echo "🛡️ Rate Limit Test - Register Endpoint"
echo "======================================="
echo "Limit: 5 requests/minute"
echo ""
echo "Sending 20 parallel requests..."
echo ""

for i in {1..20}; do
    (
        RAND=$RANDOM
        http_code=$(curl -X POST -s -o /dev/null -w "%{http_code}" \
            -H "Content-Type: application/json" \
            -d "{\"username\":\"user$RAND\",\"email\":\"test$RAND@test.com\",\"password\":\"Test123!@#\"}" \
            http://localhost:5050/auth/register)
        
        case $http_code in
            200) echo "✅ Request $i: HTTP 200 - Success" ;;
            429) echo "🛡️  Request $i: HTTP 429 - RATE LIMITED!" ;;
            *) echo "❓ Request $i: HTTP $http_code" ;;
        esac
    ) &
done

wait

echo ""
echo "======================================="
echo ""
echo "Expected: First 5 should be 200, rest should be 429"
echo "If you see 429 responses, Rate Limiting is WORKING! ✅"
