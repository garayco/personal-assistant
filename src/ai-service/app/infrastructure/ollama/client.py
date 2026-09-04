from typing import Any

import httpx

from app.core.config import settings


class OllamaClient:
    async def chat(self, messages: list[dict[str, str]]) -> str:
        payload: dict[str, Any] = {
            "model": settings.ollama_model,
            "messages": messages,
            "stream": False,
        }

        async with httpx.AsyncClient(
            base_url=settings.ollama_base_url,
            timeout=120.0,
        ) as client:
            response = await client.post("/api/chat", json=payload)
            response.raise_for_status()
            data = response.json()

        return data["message"]["content"]


ollama_client = OllamaClient()
