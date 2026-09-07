from typing import Any
from dataclasses import dataclass

import httpx

from app.core.config import settings


@dataclass
class OllamaChatResult:
    content: str
    prompt_tokens: int
    response_tokens: int


class OllamaClient:
    async def chat(
        self,
        messages: list[dict[str, str]],
    ) -> OllamaChatResult:
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

        return OllamaChatResult(
            content=data["message"]["content"],
            prompt_tokens=data.get("prompt_eval_count", 0),
            response_tokens=data.get("eval_count", 0),
        )


ollama_client = OllamaClient()
