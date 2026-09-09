import logging
import httpx

from app.application.exceptions import AiServiceUnavailableError
from app.application.prompt_builder import build_messages
from app.domain.models import AiServiceResponse, AiServiceRequest
from app.infrastructure.ollama.client import ollama_client

logger = logging.getLogger(__name__)


class ChatService:
    async def generate_response(self, request: AiServiceRequest) -> AiServiceResponse:
        logger.info(
            "chat_inference memories=%d habits=%d has_summary=%s history=%d",
            len(request.relevant_memories),
            len(request.active_habits),
            bool(request.current_summary),
            len(request.history),
        )
        try:
            result = await ollama_client.chat(build_messages(request))
        except (httpx.HTTPError, KeyError) as error:
            raise AiServiceUnavailableError from error

        return AiServiceResponse(
            answer=result.content, prompt_tokens=result.prompt_tokens
        )


chat_service = ChatService()
