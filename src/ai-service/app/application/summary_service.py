import logging
import httpx

from app.application.exceptions import AiServiceUnavailableError
from app.application.prompt_builder import build_summary
from app.domain.models import SummaryRequest, SummaryResponse
from app.infrastructure.ollama.client import ollama_client

logger = logging.getLogger(__name__)


class SummaryService:
    async def generate_summary(self, request: SummaryRequest) -> SummaryResponse:
        try:
            result = await ollama_client.chat(build_summary(request), format="json")
        except (httpx.HTTPError, KeyError) as error:
            raise AiServiceUnavailableError from error

        try:
            return SummaryResponse.model_validate_json(result.content)
        except Exception:
            logger.warning("No se pudo deserializar el JSON de resumen. Contenido: %s", result.content, exc_info=True)
            return SummaryResponse(summary=result.content)


summary_service = SummaryService()
