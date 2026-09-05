import httpx

from app.application.exceptions import AiServiceUnavailableError
from app.application.prompt_builder import build_summary
from app.domain.models import SummaryRequest, SummaryResponse
from app.infrastructure.ollama.client import ollama_client


class SummaryService:
    async def generate_summary(self, request: SummaryRequest) -> SummaryResponse:
        try:
            summary = await ollama_client.chat(build_summary(request))
        except (httpx.HTTPError, KeyError) as error:
            raise AiServiceUnavailableError from error

        return SummaryResponse(summary=summary)


summary_service = SummaryService()
