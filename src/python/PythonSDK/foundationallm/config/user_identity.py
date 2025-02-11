from pydantic import BaseModel

class UserIdentity(BaseModel):
    """
    Represents strongly-typed user identity information, regardless of
    the identity provider.
    """
    name: str = None
    user_name: str = None
    upn: str = None
 
    @staticmethod
    def from_json(json_dict):
        if json_dict == None:
            return None
        else:
            return UserIdentity(
                name=json_dict['name'],
                user_name=json_dict['user_name'],
                upn=json_dict['upn']
            )