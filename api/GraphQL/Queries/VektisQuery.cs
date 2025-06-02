using api.GraphQLTypes;
using Core.DomainServices;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Schemas
{
    public class VektisQuery : ObjectGraphType
    {
        public VektisQuery(IVektisRepository repository)
        {
            Field<ListGraphType<DiagnosisType>>(
               "diagnosis",
               resolve: context => repository.GetAllDiagnosis()
           );
            Field<ListGraphType<TreatmentTypesType>>(
               "treatmentType",
               resolve: context => repository.GetAllTreatmentTypes()
           );
        }
    }
}
