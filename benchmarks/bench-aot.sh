#!/bin/bash
# Benchmark AOT startup time for Mamba
# Usage: ./bench-aot.sh [runs]

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/aot-startup"
PUBLISH_DIR="$SCRIPT_DIR/../artifacts/publish/AotStartup/release"
BINARY="$PUBLISH_DIR/AotStartup"
RUNS=${1:-5}

echo "=== Mamba AOT Startup Benchmark ==="
echo

# Build if needed
if [ ! -f "$BINARY" ] || [ "$PROJECT_DIR/Program.cs" -nt "$BINARY" ]; then
    echo "Publishing AOT binary..."
    dotnet publish -c Release "$PROJECT_DIR" --nologo -v q
    echo
fi

echo "Binary size: $(ls -lh "$BINARY" | awk '{print $5}')"
echo
echo "Running $RUNS iterations:"
echo

BASE_PORT=15000
for i in $(seq 1 $RUNS); do
    PORT=$((BASE_PORT + i))
    OUTPUT=$("$BINARY" $PORT 2>&1)
    TOTAL=$(echo "$OUTPUT" | grep "total_startup_ms" | cut -d= -f2)
    PROCESS=$(echo "$OUTPUT" | grep "process_to_main_ms" | cut -d= -f2)
    SERVER=$(echo "$OUTPUT" | grep "server_ready_ms" | cut -d= -f2)
    
    if [ $i -eq 1 ]; then
        echo "  Run $i: ${TOTAL}ms total (${PROCESS}ms init, ${SERVER}ms server) ← cold"
    else
        echo "  Run $i: ${TOTAL}ms total (${PROCESS}ms init, ${SERVER}ms server)"
    fi
done

echo
echo "Note: Run 1 includes OS loading binary into cache."
