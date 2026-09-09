import logging
import time
from fastapi import FastAPI, Request

from app.api.routes import router

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("ai_service.http")

app = FastAPI(title="Personal Assistant AI Service", version="0.1.0")


@app.middleware("http")
async def log_requests_middleware(request: Request, call_next):
    if request.url.path == "/health":
        return await call_next(request)

    start = time.perf_counter()
    response = await call_next(request)
    elapsed_ms = (time.perf_counter() - start) * 1000
    logger.info(
        "http_request method=%s path=%s status=%d duration_ms=%.0f",
        request.method,
        request.url.path,
        response.status_code,
        elapsed_ms,
    )
    return response


app.include_router(router)
