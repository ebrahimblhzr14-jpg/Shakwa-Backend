namespace ComplaintManagement.Business.Enums
{
    public enum StatusUpdateResult
    {
        Success,            // Status updated successfully (maps to HTTP 204 NoContent)
        NotFound,           // Complaint ID not found (maps to HTTP 404 NotFound)
        AlreadyClosed,      // Complaint is already closed (maps to HTTP 409 Conflict)
        CannotModifyClosed, // Cannot modify status of a closed complaint (maps to HTTP 400 BadRequest)
        InvalidTransition   // Invalid status ID or transition rule violation (maps to HTTP 400 BadRequest)
    }
}
