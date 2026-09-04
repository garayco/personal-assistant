import httpx

from app.application.prompt_builder import build_messages
from app.domain.models import AiServiceResponse, ChatRequest
from app.infrastructure.ollama.client import ollama_client


class AiServiceUnavailableError(Exception):
    pass


class ChatService:
    async def generate_response(self, request: ChatRequest) -> AiServiceResponse:
        try:
            answer = await ollama_client.chat(build_messages(request))
        except (httpx.HTTPError, KeyError) as error:
            raise AiServiceUnavailableError from error

        return AiServiceResponse(answer=answer)


chat_service = ChatService()
