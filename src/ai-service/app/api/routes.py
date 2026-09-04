from fastapi import APIRouter, HTTPException

from app.application.chat_service import AiServiceUnavailableError, chat_service
from app.domain.models import AiServiceResponse, ChatRequest


router = APIRouter()


@router.get("/health")
def health() -> dict[str, str]:
    return {"status": "ok"}


@router.post("/chat", response_model=AiServiceResponse)
async def chat(request: ChatRequest) -> AiServiceResponse:
    try:
        print(request)
        return await chat_service.generate_response(request)
    except AiServiceUnavailableError as error:
        raise HTTPException(
            status_code=502,
            detail="Ollama no pudo procesar la solicitud.",
        ) from error
