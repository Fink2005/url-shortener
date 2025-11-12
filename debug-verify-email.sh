#!/bin/bash

echo "========================================="
echo "   DEBUG VERIFY-EMAIL ISSUE ON SERVER"
echo "========================================="
echo ""

echo "📬 [1/8] Checking MailService logs for EmailVerifiedEvent..."
docker logs mailservice --tail 100 | grep -i "verified" || echo "❌ No 'verified' logs found"
echo ""

echo "🔍 [2/8] Checking AuthService received EmailVerifiedEvent..."
docker logs authservice --tail 100 | grep -i "EmailVerifiedEvent" || echo "❌ No EmailVerifiedEvent received"
echo ""

echo "✅ [3/8] Checking AuthService published CreateUserProfileCommand..."
docker logs authservice --tail 100 | grep -i "CreateUserProfileCommand" || echo "❌ No CreateUserProfileCommand published"
echo ""

echo "👤 [4/8] Checking UserService received CreateUserProfileCommand..."
docker logs userservice --tail 100 | grep -i "CreateUserProfile" || echo "❌ UserService did not receive command"
echo ""

echo "🐰 [5/8] Checking RabbitMQ queues..."
docker exec rabbitmq rabbitmqctl list_queues name messages consumers | grep -E "auth|user|mail|saga"
echo ""

echo "🔌 [6/8] Checking RabbitMQ connections..."
docker exec rabbitmq rabbitmqctl list_connections name peer_host state | grep -E "authservice|userservice|mailservice"
echo ""

echo "💾 [7/8] Checking auth_db for email verification status..."
echo "Latest 3 users in auth_db:"
docker run --rm --network url-shortener-network postgres:16 psql "postgresql://postgres:urlShortener123@url-shortener-instance.cpicai0qavde.ap-southeast-1.rds.amazonaws.com:5432/auth_db" -c "SELECT \"Email\", \"IsEmailVerified\", \"Role\", \"CreatedAt\" FROM \"AuthUsers\" ORDER BY \"CreatedAt\" DESC LIMIT 3;"
echo ""

echo "💾 [8/8] Checking user_db for user profiles..."
echo "Latest 3 users in user_db:"
docker run --rm --network url-shortener-network postgres:16 psql "postgresql://postgres:urlShortener123@url-shortener-instance.cpicai0qavde.ap-southeast-1.rds.amazonaws.com:5432/user_db" -c "SELECT \"Username\", \"Email\", \"CreatedAt\" FROM \"Users\" ORDER BY \"CreatedAt\" DESC LIMIT 3;"
echo ""

echo "========================================="
echo "           DEBUG COMPLETED"
echo "========================================="
echo ""
echo "🔧 Next steps:"
echo "1. If MailService shows 'verified' but AuthService doesn't receive event:"
echo "   → RabbitMQ routing issue, check queue bindings"
echo ""
echo "2. If AuthService receives event but doesn't publish CreateUserProfileCommand:"
echo "   → Check AuthService error logs: docker logs authservice --tail 200"
echo ""
echo "3. If UserService doesn't receive command:"
echo "   → Check UserService consumer configuration"
echo ""
echo "4. If all events flow but user not created:"
echo "   → Check database connection and UserService handler logs"
