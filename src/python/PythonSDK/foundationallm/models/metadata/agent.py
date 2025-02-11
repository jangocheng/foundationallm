from .metadata_base import MetadataBase
from typing import Optional

class Agent(MetadataBase):
    """Agent metadata model."""
    prompt_prefix: Optional[str] = None
    prompt_suffix: Optional[str] = None