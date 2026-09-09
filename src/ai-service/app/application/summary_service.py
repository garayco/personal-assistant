import json
import logging
import httpx

from app.application.exceptions import AiServiceUnavailableError
from app.application.prompt_builder import build_summary
from app.domain.models import SummaryRequest, SummaryResponse, UpdatedFactItem
from app.infrastructure.ollama.client import ollama_client

logger = logging.getLogger(__name__)


class SummaryService:
    async def generate_summary(self, request: SummaryRequest) -> SummaryResponse:
        logger.info(
            "summary_start messages=%d session_id=%s",
            len(request.history),
            request.session_id,
        )
        try:
            result = await ollama_client.chat(build_summary(request), format="json")
        except (httpx.HTTPError, KeyError) as error:
            raise AiServiceUnavailableError from error

        try:
            response = SummaryResponse.model_validate_json(result.content)
            logger.info(
                "summary_complete new_facts=%d updated_facts=%d",
                len(response.new_facts),
                len(response.updated_facts),
            )
            return response
        except Exception:
            logger.warning(
                "summary_validation_fallback",
                exc_info=True,
            )
            try:
                data = json.loads(result.content)
                summary_text = str(data.get("summary") or "")
                raw_new_facts = data.get("new_facts") or []
                new_facts = [str(f).strip() for f in raw_new_facts if f]

                raw_updates = data.get("updated_facts") or []
                updated_facts = []
                for item in raw_updates:
                    if isinstance(item, dict) and item.get("updated"):
                        item_id = item.get("id")
                        if item_id is not None:
                            try:
                                updated_facts.append(
                                    UpdatedFactItem(
                                        id=int(item_id), updated=str(item["updated"])
                                    )
                                )
                            except (ValueError, TypeError):
                                pass

                return SummaryResponse(
                    summary=summary_text if summary_text else result.content,
                    new_facts=new_facts,
                    updated_facts=updated_facts,
                )
            except Exception:
                return SummaryResponse(summary=result.content)


summary_service = SummaryService()
