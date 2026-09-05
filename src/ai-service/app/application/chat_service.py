import httpx

from app.application.exceptions import AiServiceUnavailableError
from app.application.prompt_builder import build_messages
from app.domain.models import AiServiceResponse, AiServiceRequest
from app.infrastructure.ollama.client import ollama_client


class ChatService:
    async def generate_response(self, request: AiServiceRequest) -> AiServiceResponse:
        try:
            answer = await ollama_client.chat(build_messages(request))
        except (httpx.HTTPError, KeyError) as error:
            raise AiServiceUnavailableError from error

        return AiServiceResponse(answer=answer)


chat_service = ChatService()
