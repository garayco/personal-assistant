from fastapi import APIRouter, HTTPException

from app.application.chat_service import chat_service
from app.application.exceptions import AiServiceUnavailableError
from app.application.summary_service import summary_service
from app.infrastructure.ollama.client import ollama_client
from app.domain.models import (
    AiServiceResponse,
    AiServiceRequest,
    SummaryResponse,
    SummaryRequest,
    EmbeddingRequest,
    EmbeddingResponse,
)

router = APIRouter()


@router.get("/health")
def health() -> dict[str, str]:
    return {"status": "ok"}


@router.post("/chat", response_model=AiServiceResponse)
async def chat(request: AiServiceRequest) -> AiServiceResponse:
    try:
        return await chat_service.generate_response(request)
    except AiServiceUnavailableError as error:
        raise HTTPException(
            status_code=502,
            detail="No se pudo procesar la solicitud.",
        ) from error


@router.post("/summary", response_model=SummaryResponse)
async def summary(request: SummaryRequest) -> SummaryResponse:
    try:
        return await summary_service.generate_summary(request)
    except AiServiceUnavailableError as error:
        raise HTTPException(
            status_code=502,
            detail="No se pudo generar el resumen.",
        ) from error


@router.post("/embeddings", response_model=EmbeddingResponse)
async def embeddings(request: EmbeddingRequest) -> EmbeddingResponse:
    try:
        vector = await ollama_client.generate_embedding(request.text)
        return EmbeddingResponse(embedding=vector)
    except Exception as error:
        raise HTTPException(
            status_code=502,
            detail=f"Error generando embedding: {str(error)}",
        ) from error
