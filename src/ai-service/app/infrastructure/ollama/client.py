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
        format: str | None = None,
    ) -> OllamaChatResult:
        payload: dict[str, Any] = {
            "model": settings.ollama_model,
            "messages": messages,
            "stream": False,
        }
        if format:
            payload["format"] = format

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

    async def generate_embedding(self, text: str) -> list[float]:
        async with httpx.AsyncClient(
            base_url=settings.ollama_base_url,
            timeout=60.0,
        ) as client:
            # Primero intentamos con el endpoint moderno /api/embed
            embed_payload = {
                "model": settings.ollama_model,
                "input": text,
            }
            resp = await client.post("/api/embed", json=embed_payload)
            if resp.is_success:
                data = resp.json()
                embeddings = data.get("embeddings", [])
                if embeddings and len(embeddings) > 0:
                    return embeddings[0]

            # Fallback al endpoint clásico /api/embeddings
            legacy_payload = {
                "model": settings.ollama_model,
                "prompt": text,
            }
            resp_legacy = await client.post("/api/embeddings", json=legacy_payload)
            resp_legacy.raise_for_status()
            data_legacy = resp_legacy.json()
            return data_legacy.get("embedding", [])


ollama_client = OllamaClient()
