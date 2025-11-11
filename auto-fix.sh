#!/bin/bash

# Auto-fix common Docker build issues

echo "🔧 Auto-fixing common issues..."

# Fix 1: Lowercase libs → Libs in .csproj files
echo "✅ Fixing path references..."
find . -name "*.csproj" -type f | while read file; do
    sed -i '' 's|\.\.\\libs\\|..\\Libs\\|g' "$file" 2>/dev/null || true
    sed -i '' 's|\.\./libs/|\.\./Libs/|g' "$file" 2>/dev/null || true
done

# Fix 2: Remove containers
echo "✅ Cleaning up containers..."
docker-compose down -v 2>/dev/null || true

# Fix 3: Build without cache
echo "✅ Building services (this may take 10+ minutes)..."
docker-compose build --no-cache

# Fix 4: Start services
if [ $? -eq 0 ]; then
    echo "✅ Build successful! Starting services..."
    docker-compose up -d
    sleep 3
    echo ""
    echo "🎉 Services started!"
    docker-compose ps
else
    echo "❌ Build failed. Check logs with: docker-compose logs"
    exit 1
fi
