#!/bin/bash

echo "🔍 Testing Admin Dashboard Performance"
echo "======================================="
echo ""

# Register 10 users to simulate N+1 problem
echo "📝 Creating test users..."
for i in {1..10}; do
    echo -n "Creating user$i..."
    curl -X POST http://localhost:5050/auth/register \
      -H "Content-Type: application/json" \
      -d "{\"username\":\"perftest$i\",\"email\":\"perftest$i@test.com\",\"password\":\"Test123!@#\"}" \
      -s -o /dev/null
    echo " ✓"
    sleep 0.5
done

echo ""
echo "⏱️  Testing /admin/dashboard/users performance..."
echo ""

# Get admin token (this is mock, in real world you'd login as admin)
# For now we'll just test response time
time curl -X GET http://localhost:5050/admin/dashboard/users \
  -H "Authorization: Bearer test" \
  -s -o /dev/null -w "\n📊 HTTP Status: %{http_code}\n⏱️  Response Time: %{time_total}s\n"

echo ""
echo "📋 Check SagaService logs for detailed timing:"
echo "docker logs sagaservice 2>&1 | grep -E '(Retrieved.*users|Performance)' | tail -5"
